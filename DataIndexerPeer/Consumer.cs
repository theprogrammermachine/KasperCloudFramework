using System.Threading.Tasks;
using SharedArea;
using SharedArea.Answers;
using SharedArea.Questions;

namespace DataIndexerPeer
{
    public class Consumer : IResponder<AskIndexEntity, AnswerIndexEntity>
    {
        public AnswerIndexEntity AnswerQuestion(AnswerContext<AskIndexEntity> question)
        {
            return new AnswerIndexEntity() {Index = 1};
        }
    }
}