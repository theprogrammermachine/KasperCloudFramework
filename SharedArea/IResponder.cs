using System.Threading.Tasks;

namespace SharedArea
{
    public interface IResponder<Q, A> 
        where Q : class
        where A : class
    {
        A AnswerQuestion(AnswerContext<Q> question);
    }
}