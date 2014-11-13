using Xunit;

namespace Courier.Tests
{
    public class MediatorFactoryTests
    {

        [Fact]
        public void SingeltonMediator()
        {
            var singelton = MediatorFactory.GetStaticMediator();
            int singletonHashCode = singelton.GetHashCode();

            var secondTime = MediatorFactory.GetStaticMediator();
            int secondTimehasCode = secondTime.GetHashCode();

            Assert.Equal(singletonHashCode,secondTimehasCode);
        }

        [Fact]
        public void InstanceMediator()
        {

            var singelton = MediatorFactory.GetNewMediatorInstance();
            int singletonHashCode = singelton.GetHashCode();

            var secondTime = MediatorFactory.GetNewMediatorInstance();
            int secondTimehasCode = secondTime.GetHashCode();

            Assert.NotEqual(singletonHashCode, secondTimehasCode);
        }

    }
}
