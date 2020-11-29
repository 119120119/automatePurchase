using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

public class Processor : IDisposable
{
    private DateTime _currentMaxDate;
    private ChromeDriver _driver;
    private WebDriverWait _wait;
    private const string _reloadUrl = "https://amazon-slu.luum.com/commute/expenses";
    private const decimal _maxPerExpense = 25;
    private const decimal _maxTotalExpense = 170;

    public Processor()
    {
        _driver = new ChromeDriver();
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        var today = DateTime.Today;
        _currentMaxDate = new DateTime(today.Year, today.Month, 1);
    }
    public void Process()
    {
        try
        {
            _driver.Navigate().GoToUrl(_reloadUrl);
            _wait.Until(d => d.Title == "Expenses - Luum");

            var remain = CalculateRemaining();

            if (remain <= 0) return;

            _driver.FindElementByClassName("js-expense-form-loader").Click();
            _wait.Until(ExpectedConditions.ElementExists(By.ClassName("hasDatepicker")));

            var expenseForm = _driver.FindElementById("js-expense-form-container");
            var nextWorkDay = GetNextWorkDay();
            if (nextWorkDay > DateTime.Today) return;

            var dateInput = expenseForm.FindElement(By.ClassName("hasDatepicker"));
            _driver.ExecuteScript("arguments[0].setAttribute('value', '" + nextWorkDay.ToString("yyyy-MM-dd") + "')", dateInput);

            var typeSelect = expenseForm.FindElement(By.Id("js-expense-form-expense-type-field"));
            var selectElement = new SelectElement(typeSelect);
            selectElement.SelectByIndex(Convert.ToInt32(Random() * 5 + 1));

            expenseForm.FindElement(By.Name("amount")).SendKeys(GetNewExpenseAmount(remain));

            _driver.FindElement(By.Name("submit-button")).Click();
        }
        catch (Exception e)
        {
            // do nothing
        }
    }

    private decimal Random()
    {
        var random = new Random();
        return Convert.ToDecimal(random.NextDouble());
    }

    private string GetNewExpenseAmount(decimal remain)
    {
        var newExpense = (remain <= _maxPerExpense) ? remain : _maxPerExpense * 0.8m + _maxPerExpense * 0.2m * Random() - 0.5m;
        return Math.Round(newExpense, 2).ToString();
    }

    private decimal CalculateRemaining()
    {
        var table = _driver.FindElement(By.Id("js-expenses-table"));
        var rows = table.FindElement(By.TagName("tbody")).FindElements(By.TagName("tr"));
        var today = DateTime.Today;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
        _currentMaxDate = firstDayOfMonth;
        decimal currentTotal = 0;
        foreach (var row in rows)
        {
            var columns = row.FindElements(By.TagName("td"));
            var date = Convert.ToDateTime(columns[1].Text);
            if (date >= firstDayOfMonth)
            {
                currentTotal += Convert.ToDecimal(columns[3].Text.Replace("$", ""));
                if (date > _currentMaxDate)
                {
                    _currentMaxDate = date;
                }
            }
        }
        return _maxTotalExpense - currentTotal;
    }

    private DateTime GetNextWorkDay()
    {
        var nextWorkDay = _currentMaxDate.AddDays(1);
        while (nextWorkDay.DayOfWeek == DayOfWeek.Saturday || nextWorkDay.DayOfWeek == DayOfWeek.Sunday)
        {
            nextWorkDay = nextWorkDay.AddDays(1);
        }
        return nextWorkDay;
    }

    public void Dispose()
    {
        _driver.Close();
    }
}
