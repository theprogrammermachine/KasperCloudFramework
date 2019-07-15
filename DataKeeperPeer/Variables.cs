namespace DataKeeperPeer
{
    public class Variables
    {
        public static string ApiGatewayAddress = "localhost:9096";
        
        public static string SuperPeerAddress = "localhost:9095";
        
        public static string SelfPeerAddress = "localhost:9093";
        public static string SelfClusterCode { get; set; } = "iran";
        public static string SelfPeerCode { get; set; } = "guilan";
        public static string BugSnagToken { get; set; }
    }
}