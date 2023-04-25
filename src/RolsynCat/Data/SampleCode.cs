namespace RoslynCat.Data
{
    public class SampleCode
    {
        public List<Code> codes = new List<Code>();
        public SampleCode() {
            codes.Add(new Code() {
                Title = "类型相等",
                Sample = @"using System;


/*
对于值类型，Equals方法默认使用值类型的比较方式，即只有两个值类型的实例中的所有字段都相等时，它们才被认为是相等的。
对于引用类型，Equals方法默认使用引用类型的比较方式，即只有两个引用类型的实例是同一个对象时（即它们的引用地址相同），它们才被认为是相等的。
更好的方式是使用Object.ReferenceEquals(Object, Object) 方法，因为Equals可能被某个类重写。
但是值类型比较不能用Object.ReferenceEquals，因为会装箱
*/
class Program
{
    static void Main(string[] args)
    {
        // 值类型之间的相等性比较
        Point point = new Point()
        {
            X = 1,
            Y = 2
        };

        int x = 1;
        int y = 2;
        bool areEqual = point.Equals(new Point(){
            X = x,Y = y
        }); // 使用默认的值类型比较方式比较
        Console.WriteLine($""point.Equals(new Point() {{ X = {x}, Y = {y} }}): {areEqual}""); // 输出“True”
        // 引用类型之间的相等性比较
        Person person = new Person()
        {
            Name = ""Alice"",
            Age = 30
        };
        areEqual = person.Equals(new Person(){
            Name = person.Name,Age = person.Age
        }); // 使用默认的引用类型比较方式比较
        Console.WriteLine($""person.Equals(student): {areEqual}""); // 输出“False”
        int int1 = 3;
        //值类型比较不能用Object.ReferenceEquals，因为会装箱
        Console.WriteLine(Object.ReferenceEquals(int1, int1));//输出false
        Console.WriteLine(int1.GetType().IsValueType); 
// The example displays the following output:
//       False
//       True
    }
}

struct Point
{
    public int X;
    public int Y;
}

class Person
{
    public string Name;
    public int Age;
}"
            });
            codes.Add(new Code() {
                Title = "对象",
                Sample = @"using System;

class Program
{
    static void Main()
    {
        // 创建一个 Person 类的实例 person1，传递参数 ""Leopold"" 和 6 给构造函数
        Person person1 = new Person(""Leopold"", 6);
        Console.WriteLine(""person1 名称 = {0} 年龄 = {1}"", person1.Name, person1.Age);

        // 声明一个新的 Person 实例 person2，并将 person1 赋值给它
        Person person2 = person1;

        // 改变 person2 的名称和年龄，因为 person2 和 person1 引用同一个实例，
        // 所以 person1 也会随之改变
        person2.Name = ""Molly"";
        person2.Age = 16;

        Console.WriteLine(""person2 名称 = {0} 年龄 = {1}"", person2.Name, person2.Age);
        Console.WriteLine(""person1 名称 = {0} 年龄 = {1}"", person1.Name, person1.Age);

        Console.WriteLine(""类是引用类型，因此类对象的变量保存的是对对象的引用"");

        // 创建一个 StructPerson 结构体实例 sp1，并使用 ""new"" 初始化
        // 内存分配在线程堆栈上
        StructPerson sp1 = new StructPerson(""Alex"", 9);
        Console.WriteLine(""sp1 名称 = {0} 年龄 = {1}"", sp1.Name, sp1.Age);

        // 创建一个新的 StructPerson 结构体实例 sp2，可以不使用 ""new"" 进行初始化
        StructPerson sp2 = sp1;

        // 给 sp2 的成员变量赋值
        sp2.Name = ""Spencer"";
        sp2.Age = 7;
        Console.WriteLine(""sp2 名称 = {0} 年龄 = {1}"", sp2.Name, sp2.Age);

        // 因为 sp2 是 sp1 的副本，所以 sp1 的值不会改变
        Console.WriteLine(""sp1 名称 = {0} 年龄 = {1}"", sp1.Name, sp1.Age);
        Console.WriteLine(""结构体是值类型，因此结构体对象的变量保存的是整个对象的副本"");
    }
}

// 定义一个 StructPerson 结构体，包含两个公共成员变量 Name 和 Age，
// 并提供一个构造函数来初始化这两个成员变量
public struct StructPerson
{
    public string Name;
    public int Age;
    public StructPerson(string name, int age)
    {
        Name = name;
        Age = age;
    }
}

// 定义一个 Person 类，包含两个公共属性 Name 和 Age，
// 并提供一个构造函数来初始化这两个属性
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public Person(string name, int age)
    {
        Name = name;
        Age = age;
    }
}"
            });
        }
        public class Code
        {
            public string Title { set; get; }
            public string Sample { set; get; }
        }
    }
   
}
