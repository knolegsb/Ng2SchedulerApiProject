using AutoMapper;
using Ng2Scheduler.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ng2SchedulerApiProject.ViewModels.Mappings
{
    public class ViewModelToDomainMappingProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<ScheduleViewModel, Schedule>()
                .ForMember(s => s.Creator, map => map.UseValue(null))
                .ForMember(s => s.Attendees, map => map.UseValue(new List<Attendee>()));

                cfg.CreateMap<UserViewModel, User>();
            });
        }
    }
}
