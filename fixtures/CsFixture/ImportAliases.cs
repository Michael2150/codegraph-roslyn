// Covers: using alias, plain using, namespace, nested namespace,
//         nested class, cross-namespace object creation
using Col = System.Collections.Generic;
using IO = System.IO;
using System.Text;

namespace Fixtures.Imports
{
    public class AliasDemo
    {
        private readonly Col.List<string> _items = new();
        private readonly StringBuilder _builder = new();

        public void AddItem(string item)
        {
            _items.Add(item);
            _builder.AppendLine(item);
        }

        public string GetSummary() => _builder.ToString();
        public int    GetCount()   => _items.Count;

        public string ReadFile(string path) => IO.File.ReadAllText(path);
    }
}

namespace Fixtures.Nested
{
    using Fixtures.Imports;

    public class OuterClass
    {
        // Covers: nested (inner) class
        public class InnerClass
        {
            public void DoWork()
            {
                var helper = new AliasDemo();
                helper.AddItem("test");
            }
        }

        public InnerClass CreateInner() => new InnerClass();

        public void Run()
        {
            var inner = CreateInner();
            inner.DoWork();
        }
    }
}
