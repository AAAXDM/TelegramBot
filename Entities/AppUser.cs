
namespace WebApplication1
{
    public class AppUser
    {
        public long Id { get; private set; }
        public long ChatId { get; private set; }
        public string? Question { get; private set; }
        public int RequestNumber {get; private set; }
        public List<int> Unswers { get; private set; }

        readonly int maxRequest = 5;

        public AppUser(long chatId, long id) 
        {
            Id = id;
            ChatId = chatId;
            RequestNumber = 0;
            Unswers = new List<int>(maxRequest);
        }

        public void ResetAppUser(long chatId)
        {
            ChatId = chatId;
            ResetRequestNumber();
            ClearUnswers();
            CleqrQuestion();
        }

        public void IncreaseRequestNumber() => RequestNumber++;
        
        public void ResetRequestNumber() => RequestNumber = 0;

        public void SetUnswer(int requestnumber, int unswer) => Unswers[requestnumber] = unswer;

        public void ClearUnswers() => Unswers.Clear();

        public void AddQuestion(string question) => Question = question;
        public void CleqrQuestion() => Question = null;
    }
}
