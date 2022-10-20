using MongoDB.Bson;

namespace DataStructure
{
    public class MongoDBVariable
    {
        public ObjectId _id { get; set; }

        public long timestamp { get; set; }

        public double value { get; set; }
    }
}
