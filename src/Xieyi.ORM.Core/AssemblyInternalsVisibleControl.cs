using System.Runtime.CompilerServices;

//需要扩展的类型需要在此添加对应的程序集友元标识
[assembly: InternalsVisibleTo("Xieyi.ORM.Cache")]
[assembly: InternalsVisibleTo("Xieyi.ORM.MySQL")]
[assembly: InternalsVisibleTo("Xieyi.ORM.Validation")]
[assembly: InternalsVisibleTo("Xieyi.ORM.MongoDb")]
namespace Xieyi.ORM.Core
{
    /// <summary>
    /// 内部程序集对外可见性控制专用，没有其他实际用途
    /// </summary>
    internal class AssemblyInternalsVisibleControl
    {
    }
}