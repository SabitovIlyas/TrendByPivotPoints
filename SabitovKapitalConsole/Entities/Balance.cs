using System.Xml.Linq;

namespace SabitovCapitalConsole.Entities
{
    public class Balance
    {
        public List<BalanceStamp> BalanceStamps { get; private set; } = new List<BalanceStamp>();

        public static Balance Create()
        {
            return new Balance();
        }

        private Balance() { }

        public void Update(DateTime dateTime, decimal value)
        {
            var id = GetBalanceStampId();
            var balanceStamp = BalanceStamp.Create(dateTime, value, this, id);
            BalanceStamps.Add(balanceStamp);
            BalanceStamps = SortBalanceStamp();
        }

        private List<BalanceStamp> SortBalanceStamp()
        {
            return (from n in BalanceStamps
                    orderby n.DateTime
                    select n).ToList();
        }

        public void AddBalanceStamp(BalanceStamp balanceStamp)
        {
            BalanceStamps.Add(balanceStamp);
            BalanceStamps = SortBalanceStamp();
        }

        public bool IsThisIdAvailable(int id)
        {
            if (BalanceStamps.Find(p => p.Id == id) == null)
                return true;
            return false;
        }

        public decimal GetCurrentBalance()
        {
            if (BalanceStamps.Count == 0)
                return 0;
            return BalanceStamps.Last().Value;
        }
        public BalanceStamp GetCurrentBalanceStamp()
        {
            if (BalanceStamps.Count == 0)
                return null;    //TODO: Не возвращать null
            return BalanceStamps.Last();
        }

        public int GetBalanceStampId()
        {
            if (BalanceStamps.Count == 0)
                return 0;
            return BalanceStamps.Max(p => p.Id) + 1;
        }

        public override bool Equals(object? obj)
        {
            if (obj != null && obj.GetType() == GetType())
            {
                var balance = obj as Balance;
                if (BalanceStamps.Count != balance.BalanceStamps.Count)
                    return false;

                for (var i = 0; i < BalanceStamps.Count; i++)
                    if (BalanceStamps[i] != balance.BalanceStamps[i])
                        return false;

                return true;
            }

            return false;
        }
    }
}