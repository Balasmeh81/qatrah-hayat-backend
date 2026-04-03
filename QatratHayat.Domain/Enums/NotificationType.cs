using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QatratHayat.Domain.Enums
{
    public enum NotificationType
    {
        RequestStatus = 1,
        EligibilityReminder = 2,
        ShortageAlert = 3,
        DoctorAssignment = 4, 
        SystemAlert = 5,
        AccountAction = 6
    }
}
