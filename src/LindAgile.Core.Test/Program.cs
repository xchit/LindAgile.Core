using LindAgile.Core.Test.Commands;
using LindAgile.Core.Modules;
using LindAgile.Core.ServiceBus;

namespace LindAgile.Core.Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //注册模块
            ModuleManager.Create()
                         .UseAutofac()
                         .UseESBIoC();
            BusManager.Instance.SubscribeAll();

            BusManager.Instance.Publish(new CreateUserCommand { UserName = "zzl" });
        }
    }
}
