using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SACS.Application.Common.Interfaces;

public interface IBackgroundJobService
{
    string Enqueue<T>(Expression<Func<T, Task>> methodCall);
}
