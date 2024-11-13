using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Sui.Accounts;
using Sui.Rpc;
using Sui.Rpc.Client;
using Sui.Rpc.Models;

namespace Sui.Kiosks.Query
{
    public class KioskQuery
    {
        public static async Task<RpcResult<Kiosk.Types.PagedKioskData>> FetchKiosk
        (
            SuiClient client,
            string kioskId,
            Kiosk.Types.FetchKioskOptions options,
            int? limit = null,
            string cursor = null
        )
        {
            RpcResult<List<DynamicFieldInfo>> data =
                await Environment.KioskUtilities.GetAllDynamicFields(client, kioskId, cursor, limit);

            List<Kiosk.Types.KioskListing> listings = new List<Kiosk.Types.KioskListing>();
            List<string> lockedItemIds = new List<string>();

            Kiosk.Types.KioskData kioskData =
                Environment.KioskUtilities.ExtractKioskData(data.Result, ref listings, ref lockedItemIds, kioskId);

            Task<object>[] tasks = new Task<object>[]
            {
                options.WithKioskFields
                    ? Environment.KioskUtilities.GetKioskObject(client, kioskId).ContinueWith(task => (object)task.Result)
                    : Task.FromResult<object>(null),

                options.WithListingPrices
                    ? Environment.KioskUtilities.GetAllObjects
                      (
                          client,
                          kioskData.ListingIDs,
                          new ObjectDataOptions(show_content: true)
                      ).ContinueWith(task => (object)task.Result)
                    : Task.FromResult<object>(new List<ObjectDataResponse>()),

                options.WithObjects
                    ? Environment.KioskUtilities.GetAllObjects
                      (
                          client,
                          kioskData.ItemIDs,
                          options.SuiObjectDataOptions ?? new ObjectDataOptions(show_display: true)
                      ).ContinueWith(task => (object)task.Result)
                    : Task.FromResult<object>(new List<ObjectDataResponse>())
            };

            // Await all tasks
            await Task.WhenAll(tasks);

            // Deconstruct the results
            Kiosk.Kiosk kiosk = (Kiosk.Kiosk)tasks[0].Result;
            List<ObjectDataResponse> listingObjects = (List<ObjectDataResponse>)tasks[1].Result;
            List<ObjectDataResponse> items = (List<ObjectDataResponse>)tasks[2].Result;

            if (options.WithKioskFields)
                kioskData.Kiosk = kiosk;

            // attach items listings. IF we have `options.withListingPrices === true`, it will also attach the prices.
            Environment.KioskUtilities.AttachListingsAndPrice(ref kioskData, ref listings, listingObjects);
            // add `locked` status to items that are locked.
            Environment.KioskUtilities.AttachLockedItems(ref kioskData, lockedItemIds);

            // Attach the objects for the queried items.
            Environment.KioskUtilities.AttachObjects
            (
                ref kioskData,
                items.Where(x => x.Data != null).Select(x => x.Data).ToList()
            );

            Kiosk.Types.PagedKioskData result = new Kiosk.Types.PagedKioskData
            (
                Data: kioskData,
                NextCursor: null,
                HasNextPage: false
            );

            return new RpcResult<Kiosk.Types.PagedKioskData>(result);
        }

        public static async Task<RpcResult<Kiosk.Types.OwnedKiosks>> GetOwnedKiosks
        (
            SuiClient client,
            string address,
            string personalKioskType = null,
            int? limit = null,
            string cursor = null
        )
        {
            if (!Utilities.Utils.IsValidSuiAddress(address))
                return new RpcResult<Kiosk.Types.OwnedKiosks>
                (
                    new Kiosk.Types.OwnedKiosks
                    (
                        KioskIDs: new string[] { },
                        KioskOwnerCaps: new Kiosk.Types.KioskOwnerCap[] { },
                        HasNextPage: false,
                        NextCursor: null
                    )
                );

            List<IObjectDataFilter> filters = new List<IObjectDataFilter>
        {
            new ObjectDataFilterStructType(KioskConstants.KioskOwnerCap)
        };

            if (personalKioskType != null)
                filters.Add
                (
                    new ObjectDataFilterStructType(personalKioskType)
                );

            ObjectDataFilterMatchAny filter = new ObjectDataFilterMatchAny
            (
                filters.ToArray()
            );

            RpcResult<PaginatedObjectDataResponse> ownedObjects = await client.GetOwnedObjectsAsync
            (
                owner: AccountAddress.FromHex(address),
                filter: new ObjectQuery
                (
                    cursor,
                    limit,
                    object_data_options: new ObjectDataOptions(show_content: true, show_type: true),
                    object_data_filter: filter
                )
            );

            List<string> kioskIdList = ownedObjects.Result.Data.Select
            (
                x =>
                {
                    Newtonsoft.Json.Linq.JToken fields = x.Data.Content.Type == DataType.MoveObject ?
                        ((ParsedMoveObject)x.Data.Content.ParsedData).Fields :
                        null;

                    return (string)(fields["cap"]?["fields"]?["for"] ?? fields["for"]);
                }
            ).ToList();

            List<ObjectData> filteredData =
                ownedObjects.Result.Data.Where(x => x.Data != null).Select(x => x.Data).ToList();

            return new RpcResult<Kiosk.Types.OwnedKiosks>
            (
                new Kiosk.Types.OwnedKiosks
                (
                    KioskOwnerCaps: filteredData.Select((x, idx) =>
                    {
                        return new Kiosk.Types.KioskOwnerCap
                        (
                            IsPersonal: x.Type.ToString() != KioskConstants.KioskOwnerCap,
                            ObjectID: x.ObjectID.KeyHex,
                            KioskID: kioskIdList[idx],
                            Digest: x.Digest,
                            Version: x.Version.ToString()
                        );
                    }).ToArray(),
                    KioskIDs: kioskIdList.ToArray(),
                    HasNextPage: ownedObjects.Result.HasNextPage,
                    NextCursor: ownedObjects.Result.NextCursor
                )
            );
        }

        public static async Task<RpcResult<Kiosk.Types.IKioskExtension>> FetchKioskExtension
        (
            SuiClient client,
            string kioskId,
            string extensionType
        )
        {
            RpcResult<ObjectDataResponse> extension = await client.GetDynamicFieldObjectAsync
            (
                parent_object_id: AccountAddress.FromHex(kioskId),
                name: new DynamicFieldNameInput
                (
                    $"0x2::kiosk_extension::ExtensionKey<{extensionType}>",
                    "{ dummy_field: false }"
                )
            );

            if (extension.Result.Data == null)
                return new RpcResult<Kiosk.Types.IKioskExtension>(null);

            Newtonsoft.Json.Linq.JToken fieldsToken =
                (extension?.Result.Data?.Content.ParsedData as ParsedMoveObject)?.Fields?["value"]?["fields"];

            return new RpcResult<Kiosk.Types.IKioskExtension>
            (
                new Kiosk.Types.KioskExtensionFull
                (
                    ObjectID: extension?.Result.Data?.ObjectID.KeyHex,
                    Type: extensionType,
                    Permissions: fieldsToken?["permissions"].Value<string>(),
                    StorageID: fieldsToken?["storage"]?["fields"]?["id"]?["id"].Value<string>(),
                    IsEnabled: fieldsToken?["is_enabled"].Value<bool>(),
                    StorageSize: fieldsToken?["storage"]?["fields"]?["size"].Value<ulong>()
                )
            );
        }
    }
}