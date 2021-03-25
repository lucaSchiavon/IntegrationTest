using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using Xunit;

namespace EmployeeApp.AutomatedUITestws
{
    public class AutomatedUITestsBetterCode: IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly EmployeePage _page;
        public AutomatedUITestsBetterCode()
        {
            _driver = new ChromeDriver(@"C:\LS_ChromeDriver");
            _page = new EmployeePage(_driver);
            _page.Navigate();
        }
        [Fact]
        public void Create_WhenExecuted_ReturnsCreateView()
        {
            Assert.Equal("Create - EmployeesApp", _page.Title);
            Assert.Contains("Please provide a new employee data", _page.Source);
        }
        [Fact]
        public void Create_WrongModelData_ReturnsErrorMessage()
        {
            _page.PopulateName("New Name");
            _page.PopulateAge("35");
            _page.ClickCreate();
            Assert.Equal("Account number is required", _page.AccountNumberErrorMessage);
        }
        [Fact]
        public void Create_WhenSuccessfullyExecuted_ReturnsIndexViewWithNewEmployee()
        {
            _page.PopulateName("New Name");
            _page.PopulateAge("35");
            _page.PopulateAccountNumber("124-9384613085-58");
            _page.ClickCreate();
            Assert.Equal("Index", _page.Title);
            Assert.Contains("New Name", _page.Source);
            Assert.Contains("35", _page.Source);
            Assert.Contains("124-9384613085-58", _page.Source);
        }
        public void Dispose()
        {
            _driver.Quit();
            _driver.Dispose();
        }
    }
}
