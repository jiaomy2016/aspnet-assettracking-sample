﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Graph;
using Newtonsoft.Json;

namespace AssetTracking.Models
{
    public class OfficeItemRepository : IOfficeItemRepository
    {
        private readonly string siteId = "m365b267815.sharepoint.com,6e1261a1-6d03-432a-95c0-e1c7705aef5f,f43d258c-ece0-476a-a1c0-018d359817d5";
        private ISiteListsCollectionPage officeItemsLists;
        private readonly string itemId = null;
        public GraphServiceClient GraphClient { get; private set; }
        public async Task<List<OfficeItem>> GetItems(GraphServiceClient graphClient)
        {
            officeItemsLists = await Sites.GetLists(graphClient, siteId);
            List<OfficeItem> _officeItemDirectoryList = new List<OfficeItem>();
            if (officeItemsLists != null)
            {
                var _officeItemList = officeItemsLists.Where(x => x.DisplayName.Contains("Office Items")).FirstOrDefault();
                var listId = _officeItemList.Id;
                var _officeItems = await Sites.GetListItems(graphClient, siteId, listId);

                foreach (var item in _officeItems)
                {
                    var resourceList = item.Fields.AdditionalData;
                    var jsonString = JsonConvert.SerializeObject(resourceList);

                    var officeResource = JsonConvert.DeserializeObject<OfficeItem>(jsonString);
                    officeResource.ItemId = item.Id;
                    _officeItemDirectoryList.Add(officeResource);
                }
            }
            return _officeItemDirectoryList;
        }
        public async Task<bool> AddItem(OfficeItem officeItem, GraphServiceClient graphClient)
        {

            officeItemsLists = await Sites.GetLists(graphClient, siteId);

            if (officeItemsLists != null)
            {
                var additem = officeItemsLists.Where(b => b.DisplayName.Contains("Item")).FirstOrDefault();
                string listId = additem.Id;
                IDictionary<string, object> data = new Dictionary<string, object>
                {
                    {"OfficeItemID", officeItem.ItemId},
                    {"Title", officeItem.Title },
                    {"Resource_x0020_IDLookupId", officeItem.ResourceId },
                    {"SerialNo", officeItem.SerialNo },
                    {"Description", officeItem.ItemDescription }
                };
                bool addofficeitem = await Sites.AddListItem(graphClient, siteId,
                                                      listId,
                                                      data);
                return addofficeitem;
            }
            else
            {
                return false;
            }
        }
        public async Task<bool> UpdateItem(OfficeItem officeItem, GraphServiceClient graphClient)
        {
            officeItemsLists = await Sites.GetLists(graphClient, siteId);
            string userItemId = officeItem.ItemId;

            if (officeItemsLists != null)
            {
                var addItem = officeItemsLists.Where(b => b.DisplayName.Contains("Office Items")).FirstOrDefault();
                string _listId = addItem.Id;

                string itemId = userItemId;

                IDictionary<string, object> data = new Dictionary<string, object>
                {

                    {"OfficeItemID", officeItem.ItemId},
                    {"Title", officeItem.Title },
                    {"Resource_x0020_IDLookupId", officeItem.ResourceId },
                    {"SerialNo", officeItem.SerialNo },
                    {"Description", officeItem.ItemDescription }
                };
                bool updatebook = await Sites.UpdateListItem(graphClient, siteId,
                                                      _listId, itemId,
                                                      data);
                return updatebook;
            }
            else
            {
                return false;
            }
        }
        public async Task<bool> DeleteItem(OfficeItem officeItem, GraphServiceClient graphClient)
        {
            officeItemsLists = await Sites.GetLists(graphClient, siteId);
            string userItemId = officeItem.ItemId;

            if (officeItemsLists != null)
            {
                var addItem = officeItemsLists.Where(b => b.DisplayName.Contains("Office Items")).FirstOrDefault();
                string _listId = addItem.Id;
                string itemId = userItemId;

                bool deletebook = await Sites.DeleteListItem(graphClient, siteId,
                                                      _listId, itemId);
                return deletebook;
            }
            else
            {
                return false;
            }
        }
    }


}