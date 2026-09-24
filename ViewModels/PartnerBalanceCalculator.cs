namespace Recyclage.ViewModels;

internal static class PartnerBalanceCalculator
{
    public static void ApplyRunningBalances(IEnumerable<PartnerTransactionEntryRowViewModel> rows)
    {
        decimal leftToAyoub = 0;
        decimal leftToMustafa = 0;

        foreach (var row in rows.Where(r => r.Id > 0).OrderBy(r => r.Date).ThenBy(r => r.Id))
        {
            leftToAyoub += row.PaidByAyoub;
            leftToMustafa += row.ReturnedToMustafa;
            row.SetBalances(leftToAyoub, leftToMustafa);
        }

        foreach (var row in rows.Where(r => r.Id == 0))
            row.SetBalances(0, 0);
    }
}
