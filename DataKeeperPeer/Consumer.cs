using System.Threading.Tasks;
using SharedArea;
using SharedArea.Answers;
using SharedArea.Questions;

namespace DataKeeperPeer
{
    public class Consumer : IResponder<AskTest, AnswerTest>
    {
        public AnswerTest AnswerQuestion(AnswerContext<AskTest> question)
        {
            return new AnswerTest() {MsgText = $"Hello {question.Question.Name} !"};
        }
    }
}