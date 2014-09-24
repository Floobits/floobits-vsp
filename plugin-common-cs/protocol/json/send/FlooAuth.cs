namespace Floobits.Common.Protocol.Json.Send
{
    public class FlooAuth : InitialBase
    {
        // TODO: Share this code with NewAccount
        public string username;
        public string api_key;
        public string secret;

        public string room;
        public string room_owner;
        public string[] supported_encodings = { "utf8", "base64" };

        public FlooAuth(string username, string api_key, string secret, string owner, string workspace)
        {
            this.username = username;
            this.api_key = api_key;
            this.secret = secret;
            this.room = workspace;
            this.room_owner = owner;
        }
    }
}
