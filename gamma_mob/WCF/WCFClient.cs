using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace gamma_mob.WCF
{
    public class PrinterClient : System.ServiceModel.ClientBase<ICalculator>, ICalculator
    {
        public CalculatorClient() { }

        public CalculatorClient(string configurationName) :
            base(configurationName)
        { }

        public CalculatorClient(System.ServiceModel.Binding binding) :
            base(binding)
        { }

        public CalculatorClient(System.ServiceModel.EndpointAddress address,
        System.ServiceModel.Binding binding) :
            base(address, binding)
        { }

        public double Add(double n1, double n2)
        {
            return base.InnerChannel.Add(n1, n2);
        }
    }

}
