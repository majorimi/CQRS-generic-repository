using Majorsoft.CQRS.Repositories.Specification;
using Majorsoft.CQRS.Repositories.Tests.TestDb;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Majorsoft.CQRS.Repositories.Tests.Specification
{
    [TestClass]
    public class OrderOptionTest
    {
        [TestMethod]
        public void OrderOption_ToString_should_return_custom_value()
        {
            var order = new OrderOption<Event>(x => x.CreatedDate, false);

            var str = order.ToString();

            Assert.AreEqual("OrderOption_OrderBy:x => Convert(x.CreatedDate, Object)_Descending:False", str);
        }
    }
}
