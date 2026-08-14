using S1API.Money;
using S1Lua.Hosting;

namespace S1Lua.Runtime;

internal sealed class S1ApiMoneyService
{
    internal MoneySnapshot Get() => new(
        Money.GetCashBalance(),
        Money.GetOnlineBalance(),
        Money.GetNetWorth());

    internal void ChangeCash(double amount, bool visualizeChange, bool playCashSound) =>
        Money.ChangeCashBalance((float)amount, visualizeChange, playCashSound);
}
