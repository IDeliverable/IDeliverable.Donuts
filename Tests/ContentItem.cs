using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.ContentManagement.Records;

namespace IDeliverable.Donuts.Tests
{
    public class ContentItem
    {
        public static Orchard.ContentManagement.ContentItem WithPart<TPart, TRecord>(TPart part, string contentType, int id = -1)
            where TPart : ContentPart<TRecord>
            where TRecord : ContentPartRecord, new()
        {

            part.Record = new TRecord();
            return WithPart(part, contentType, id);
        }

        public static Orchard.ContentManagement.ContentItem WithPart<TPart>(TPart part, string contentType, int id = -1)
            where TPart : ContentPart
        {

            var contentItem = part.ContentItem = new Orchard.ContentManagement.ContentItem
            {
                VersionRecord = new ContentItemVersionRecord
                {
                    ContentItemRecord = new ContentItemRecord()
                },
                ContentType = contentType
            };

            contentItem.Record.Id = id;
            contentItem.Weld(part);
            contentItem.Weld(new InfosetPart());

            return contentItem;
        }

        public class NullContentPart : ContentPart
        {
            
        }
    }
}
