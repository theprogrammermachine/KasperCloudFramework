namespace SharedArea.Wrappers
{
    public class MessageWrapper<T>
    {
        public enum WrapperType
        {
            Question, Answer, Notification, OutNetNotif
        }
        
        public string DestClusterCode { get; set; }
        public string DestPeerCode { get; set; }
        public string SrcClusterCode { get; set; }
        public string SrcPeerCode { get; set; }
        public T Message { get; set; }
        public string QuestionId { get; set; }
        public WrapperType MessageType { get; set; }
    }
}