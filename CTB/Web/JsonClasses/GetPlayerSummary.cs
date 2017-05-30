using Newtonsoft.Json;

namespace CTB.Web.JsonClasses
{
    /// <summary>
    /// Class to serialize and deserialize a summary of the profile of a steamuser
    /// JsonProperty gets the result default values and parses it into our variables
    /// </summary>
    public class GetPlayerSummary
    {
        [JsonProperty("steamid")]
        public string SteamID { get; set; }

        [JsonProperty("communityvisibilitystate")]
        public int CommunityVisibilityState { get; set; }

        [JsonProperty("profilestate")]
        public int ProfileState { get; set; }

        [JsonProperty("personaname")]
        public string PersonaName { get; set; }

        [JsonProperty("lastlogoff")]
        public ulong LastLogOff { get; set; }

        [JsonProperty("commentpermission")]
        public int CommentPermission { get; set; }

        [JsonProperty("profileurl")]
        public string ProfileUrl { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        [JsonProperty("avatarmedium")]
        public string AvatarMedium { get; set; }

        [JsonProperty("avatarfull")]
        public string AvatarFull { get; set; }

        [JsonProperty("personastate")]
        public int PersonaState { get; set; }

        [JsonProperty("primaryclanid")]
        public string PrimaryClanID { get; set; }

        [JsonProperty("timecreated")]
        public ulong TimeCreated { get; set; }

        [JsonProperty("personastateflags")]
        public int PersonaStateFlags { get; set; }
    }
}
