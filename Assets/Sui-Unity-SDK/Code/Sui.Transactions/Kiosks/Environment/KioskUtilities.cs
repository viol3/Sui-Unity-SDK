using System.Threading.Tasks;
using Sui.Rpc;
using Sui.Rpc.Client;
using Sui.Accounts;
using OpenDive.BCS;
using Sui.Rpc.Models;
using Chaos.NaCl;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Sui.Types;
using System;

namespace Sui.Kiosks.Environment
{
    public class KioskUtilities
    {
        public static int DefaultQueryLimit = 50;

        public static async Task<RpcResult<Kiosk.Kiosk>> GetKioskObject
        (
            SuiClient client,
            string id
        )
        {
            RpcResult<Rpc.Models.ObjectDataResponse> queryRes = await client.GetObjectAsync
            (
                object_id: AccountAddress.FromHex(id),
                options: new Rpc.Models.ObjectDataOptions(show_bcs: true)
            );

            if (queryRes.Error != null)
                return new RpcResult<Kiosk.Kiosk>(null, new RpcError(-1, $"Kiosk {id} not found; {queryRes.Error}", null));

            if (queryRes.Result.Data.BCS == null || queryRes.Result.Data.BCS.Type == Rpc.Models.RawDataType.Package)
                return new RpcResult<Kiosk.Kiosk>(null, new RpcError(-1, $"Invalid kiosk query: {id}, expected object, got package", null));

            Deserialization der = new Deserialization
            (
                CryptoBytes.FromBase64String(((MoveObjectRawData)queryRes.Result.Data.BCS.Data).BCSBytes)
            );

            return new RpcResult<Kiosk.Kiosk>((Kiosk.Kiosk)Kiosk.Kiosk.Deserialize(der));
        }

        public static Kiosk.Types.KioskData ExtractKioskData
        (
            List<DynamicFieldInfo> data,
            ref List<Kiosk.Types.KioskListing> listings,
            ref List<string> lockedItemIds,
            string kioskId
        )
        {
            List<Kiosk.Types.KioskListing> listingsOut = new List<Kiosk.Types.KioskListing>();
            List<string> lockedItemIdsOut = new List<string>();
            Kiosk.Types.KioskData val = data.Aggregate
            (
                new Kiosk.Types.KioskData(),
                (Kiosk.Types.KioskData acc, DynamicFieldInfo value) =>
                {
                    string type = value.Name.Type;

                    if (type.StartsWith("0x2::kiosk::Item"))
                    {
                        acc.ItemIDs.Add(value.ObjectID.KeyHex);
                        acc.Items.Add
                        (
                            new Kiosk.Types.KioskItem
                            (
                                ObjectID: value.ObjectID.KeyHex,
                                Type: value.ObjectType,
                                IsLocked: false,
                                KioskID: kioskId
                            )
                        );
                    }

                    if (type.StartsWith("0x2::kiosk::Listing"))
                    {
                        acc.ListingIDs.Add(value.ObjectID.KeyHex);
                        listingsOut.Add
                        (
                            new Kiosk.Types.KioskListing
                            (
                                ObjectID: JToken.Parse(value.Name.Value.ToString())["id"]?.ToString(),
                                ListingID: value.ObjectID.KeyHex,
                                IsPurchaseCapIssued: (bool)JToken.Parse(value.Name.Value.ToString())["is_exclusive"]
                            )
                        );
                    }

                    if (type.StartsWith("0x2::kiosk::Lock"))
                    {
                        lockedItemIdsOut.Add
                        (
                            JToken.Parse(value.Name.Value.ToString())["id"]?.ToString()
                        );
                    }

                    if (type.StartsWith("0x2::kiosk_extension::ExtensionKey"))
                    {
                        acc.Extensions.Add
                        (
                            new Kiosk.Types.KioskExtensionOverview
                            (
                                ObjectID: value.ObjectID.KeyHex,
                                Type: SuiStructTag.FromStr(value.Name.Type).TypeArguments[0].ToString()
                            )
                        );
                    }

                    return acc;
                }
            );

            lockedItemIds = lockedItemIdsOut;
            listings = listingsOut;
            return val;
        }

        public static void AttachListingsAndPrice
        (
            ref Kiosk.Types.KioskData kioskData,
            ref List<Kiosk.Types.KioskListing> listings,
            List<ObjectDataResponse> listingObjects 
        )
        {
            Dictionary<string, Kiosk.Types.KioskListing> itemListings = listings.Select
            (
                (list, idx) => new Tuple<Kiosk.Types.KioskListing, int>(list, idx)
            ).ToList().Aggregate
            (
                new Dictionary<string, Kiosk.Types.KioskListing>(),
                (Dictionary<string, Kiosk.Types.KioskListing> acc, Tuple<Kiosk.Types.KioskListing, int> val) =>
                {
                    acc[val.Item1.ObjectID] = val.Item1;

                    if (listingObjects.Count == 0)
                        return acc;

                    Data content = listingObjects[val.Item2].Data.Content;
                    JToken data = content.Type == DataType.MoveObject ? ((ParsedMoveObject)content.ParsedData).Fields : null;

                    if (data == null)
                        return acc;

                    acc[val.Item1.ObjectID].Price = data["value"].ToString();
                    return acc;
                }
            );

            kioskData.Items.ForEach((item) =>
            {
                item.Listing = itemListings[item.ObjectID];
            });
        }

        public static void AttachObjects
        (
            ref Kiosk.Types.KioskData kioskData,
            List<ObjectData> objects
        )
        {
            Dictionary<string, ObjectData> mapping = objects.Aggregate
            (
                new Dictionary<string, ObjectData>(),
                (Dictionary<string, ObjectData> acc, ObjectData obj) =>
                {
                    acc[obj.ObjectID.KeyHex] = obj;
                    return acc;
                }
            );

            kioskData.Items.ForEach((kiosk) =>
            {
                kiosk.Data = mapping[kiosk.ObjectID];
            });
        }

        public static void AttachLockedItems
        (
            ref Kiosk.Types.KioskData kioskData,
            List<string> lockedItemIds
        )
        {
            Dictionary<string, bool> lockedStatuses = lockedItemIds.Aggregate
            (
                new Dictionary<string, bool>(),
                (Dictionary<string, bool> acc, string item) =>
                {
                    acc[item] = true;
                    return acc;
                }
            );

            kioskData.Items.ForEach((item) =>
            {
                item.IsLocked = lockedStatuses[item.ObjectID];
            });
        }

        public static async Task<RpcResult<List<DynamicFieldInfo>>> GetAllDynamicFields
        (
            SuiClient client,
            string parentId,
            string cursor = null,
            int? limit = null
        )
        {
            bool hasNextPage = true;
            List<DynamicFieldInfo> data = new List<DynamicFieldInfo>();

            while (hasNextPage)
            {
                RpcResult<PaginatedDynamicFieldInfo> result = await client.GetDynamicFieldsAsync
                (
                    parent_object_id: AccountAddress.FromHex(parentId),
                    new ObjectQuery
                    (
                        cursor: cursor,
                        limit: limit
                    )
                );

                data.AddRange(result.Result.Data);
                hasNextPage = result.Result.HasNextPage;
                cursor = result.Result.NextCursor;
            }

            return new RpcResult<List<DynamicFieldInfo>>(data);
        }

        public static RpcResult<List<ObjectDataResponse>> GetAllObjects
        (
            SuiClient client,
            List<string> ids,
            ObjectDataOptions options,
            int? limit = null
        )
        {
            int limitArg = limit ?? DefaultQueryLimit;
            List<List<string>> chunks = GetChunks(ids, limitArg);

            List<Task<List<ObjectDataResponse>>> resultsTasks = chunks.Select(async (chunk) =>
            {
                List<AccountAddress> chunk_accounts = chunk.Select((account) =>
                    AccountAddress.FromHex(account)
                ).ToList();
                RpcResult<IEnumerable<ObjectDataResponse>> res = await client.MultiGetObjectsAsync
                (
                    object_ids: chunk_accounts,
                    options: options
                );
                return res.Result.ToList();
            }).ToList();

            List<ObjectDataResponse> result = new List<ObjectDataResponse>();

            resultsTasks.ForEach(async (task) =>
            {
                List<ObjectDataResponse> res = await task;
                result.AddRange(res);
            });

            return new RpcResult<List<ObjectDataResponse>>(result);
        }

        public static async Task<RpcResult<List<ObjectDataResponse>>> GetAllOwnedObjects
        (
            SuiClient client,
            string owner,
            IObjectDataFilter filter = null,
            int? limit = null,
            ObjectDataOptions options = null
        )
        {
            int limitArg = limit ?? DefaultQueryLimit;
            ObjectDataOptions optionsArg = options ?? new ObjectDataOptions();
            optionsArg.ShowType = true;
            optionsArg.ShowContent = true;

            bool hasNextPage = true;
            string cursor = null;
            List<ObjectDataResponse> data = new List<ObjectDataResponse>();

            while (hasNextPage)
            {
                RpcResult<PaginatedObjectDataResponse> result = await client.GetOwnedObjectsAsync
                (
                    owner: AccountAddress.FromHex(owner),
                    new ObjectQuery
                    (
                        cursor: cursor,
                        limit: limitArg,
                        object_data_options: optionsArg,
                        object_data_filter: filter
                    )
                );
                data.AddRange(result.Result.Data.ToList());
                hasNextPage = result.Result.HasNextPage;
                cursor = result.Result.NextCursor;
            }

            return new RpcResult<List<ObjectDataResponse>>(data);
        }

        private static List<List<string>> GetChunks(List<string> ids, int limit)
        {
            int totalChunks = (int)Math.Ceiling((double)ids.Count / limit);
            List<List<string>> chunks = new List<List<string>>();

            for (int index = 0; index < totalChunks; index++)
            {
                List<string> chunk = ids.Skip(index * limit).Take(limit).ToList();
                chunks.Add(chunk);
            }

            return chunks;
        }
    }
}