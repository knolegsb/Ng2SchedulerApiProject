using Ng2Scheduler.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ng2Scheduler.Data.Abstract
{
    public interface IScheduleRepository : IEntityBaseRepository<Schedule>
    {
    }

    public interface IUserRepository : IEntityBaseRepository<User>
    {

    }

    public interface IAttendeeRepository : IEntityBaseRepository<Attendee>
    {

    }
}
