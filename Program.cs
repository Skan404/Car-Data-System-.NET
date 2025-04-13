using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Lab_3
{
    [XmlType(TypeName = "car")]
    public class Car
    {
        public string Model { get; set; }
        public int Year { get; set; }

        [XmlElement(ElementName = "Engine")]
        public Engine Motor { get; set; }

        public Car()
        {
            Model = "";
            Year = 0;
            Motor = null;
        }

        public Car(string model, Engine engine, int year)
        {
            Model = model;
            Motor = engine;
            Year = year;
        }
    }

    public class Engine
    {
        public double Displacement { get; set; }
        public double HorsePower { get; set; }

        [XmlAttribute]
        public string Model { get; set; }

        public Engine()
        {
            Displacement = 0.0;
            HorsePower = 0.0;
            Model = "";
        }

        public Engine(double displacement, double horsePower, string model)
        {
            Displacement = displacement;
            HorsePower = horsePower;
            Model = model;
        }
    }

    class Program
	{
        private static void createXmlFromLinq(List<Car> myCars)
        {
            IEnumerable<XElement> nodes = from car in myCars
                                          select new XElement("car",
                                            new XElement("Model", car.Model),
                                            new XElement("Year", car.Year),
                                            new XElement("Engine",
                                                new XAttribute("Model", car.Motor.Model),
                                                new XElement("Displacement", car.Motor.Displacement),
                                                new XElement("HorsePower", car.Motor.HorsePower)
                                            )
                                          );
            XElement rootNode = new XElement("cars", nodes);
            rootNode.Save("CarsFromLinq.xml");
        }
        static void Main(string[] args)
		{
			List<Car> myCars = new List<Car>()
			{
				new Car("E250", new Engine(1.8, 204, "CGI"), 2009),
				new Car("E350", new Engine(3.5, 292, "CGI"), 2009),
				new Car("A6", new Engine(2.5, 187, "FSI"), 2012),
				new Car("A6", new Engine(2.8, 220, "FSI"), 2012),
				new Car("A6", new Engine(3.0, 295, "TFSI"), 2012),
				new Car("A6", new Engine(2.0, 175, "TDI"), 2011),
				new Car("A6", new Engine(3.0, 309, "TDI"), 2011),
				new Car("S6", new Engine(4.0, 414, "TFSI"), 2012),
				new Car("S8", new Engine(4.0, 513, "TFSI"), 2012)
			};

			var linq1 = from m in myCars
						 where m.Model == "A6"
						 select new
						 {
							 engineType = m.Motor.Model == "TDI" ? "diesel" : "petrol",
							 hppl = m.Motor.HorsePower / m.Motor.Displacement
						 }
						 into obj
						 select obj;

			var linq1_2 = from m in linq1
						 group m.hppl by m.engineType;

			foreach(var i in linq1_2)
			{
				double result = 0.0;
				int c = 0;
				foreach(var value in i)
				{
					result += value;
					++c;
				}

				Console.WriteLine("{0}: {1}", i.Key, result / c);
			}

			// Zad 2
			XmlSerializer serializer = new XmlSerializer(myCars.GetType(), new XmlRootAttribute("cars"));
			using (TextWriter tw = new StreamWriter("CarsCollection.xml"))
			{
				serializer.Serialize(tw, myCars);
			}

			// Zad 3
			XElement rootNode = XElement.Load("CarsCollection.xml");
			double avgHP = (double) rootNode.XPathEvaluate("sum(car/Engine[@Model != \"TDI\"]/HorsePower) div count(car/Engine[@Model != \"TDI\"])");

			Console.WriteLine("avgHP = " + avgHP);

			IEnumerable<XElement> models = rootNode.XPathSelectElements("car[not(model = following::car/model)]/model");

            foreach (XElement model in models)
            {
                Console.WriteLine(model.Value);
            }


            // Zad 4
            createXmlFromLinq(myCars);

			// Zad 5
			XDocument xmlFile = XDocument.Load("template.html");

			var body = xmlFile.Root.LastNode as XElement;
			IEnumerable<XElement> nodes = from car in myCars
										  select new XElement("tr",
											new XElement("td", car.Model),
											new XElement("td", car.Motor.Model),
											new XElement("td", car.Motor.Displacement),
											new XElement("td", car.Motor.HorsePower),
											new XElement("td", car.Year)
										  );

			body.Add(new XElement("table", new XAttribute("border", 1), nodes));
			xmlFile.Save("CarsTable.html");

			// Zad 6
			XDocument zadanie6xml = XDocument.Load("CarsCollection.xml");

			var zad6_1 = from c in zadanie6xml.Element("cars").Elements("car").Elements("Engine").Elements("HorsePower")
						select c;

			foreach (XElement hp in zad6_1)
			{
				hp.Name = "HP";
			}

			var zad6_2 = from c in zadanie6xml.Element("cars").Elements("car")
						 select c;

			foreach (XElement car in zad6_2)
			{
				car.Element("Model").Add(new XAttribute("Year", car.Element("Year").Value));
				car.Element("Year").Remove();
			}

			zadanie6xml.Save("CarsZad6.xml");

			Console.ReadKey();
		}


	}
}

