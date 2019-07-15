namespace SharedArea
{
    public interface Answerable
    {
        void NotifyAnswerReceived(string questionId, object answer);
        void SendAnswerToQuestionaire(string clusterCode, string peerCode, string questionId, object answer);
    }
}