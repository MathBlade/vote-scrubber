using Newtonsoft.Json;
using System;

public static class VoteAPIAllData
{

    public partial class MasterData
    {
        [JsonProperty("data")]
        public VoteJSON[] Votes { get; set; }

        [JsonProperty("next")]
        public long Next { get; set; }

        [JsonProperty("prev")]
        public object Prev { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }
    }

    public partial class VoteJSON
    {
        [JsonProperty("bbcode_bitfield")]
        public string BbcodeBitfield { get; set; }

        [JsonProperty("bbcode_uid")]
        public string BbcodeUid { get; set; }

        [JsonProperty("forum_id")]
        public string ForumId { get; set; }

        [JsonProperty("post_id")]
        public string PostId { get; set; }

        [JsonProperty("post_text")]
        public string PostText { get; set; }

        [JsonProperty("poster_id")]
        public string PosterId { get; set; }

        [JsonProperty("topic_id")]
        public string TopicId { get; set; }

        public int PostNumber {  get
            {
                return int.Parse(PostId);
            } }

        
    }
}
