using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Enums
{
    
        public enum EmployeeStatus
        {
            Active = 1,
            Inactive = 2,
            OnLeave = 3,
            Terminated = 4
        }

        public enum Gender
        {
            Male = 1,
            Female = 2,
            Other = 3
        }

        public enum MaritalStatus
        {
            Single = 1,
            Married = 2,
            Divorced = 3,
            Widowed = 4
        }

        public enum PositionLevel
        {
            Junior = 1,
            Mid = 2,
            Senior = 3,
            Lead = 4,
            Manager = 5,
            Director = 6
        }

        public enum AttendanceStatus
        {
            Present = 1,
            Absent = 2,
            Late = 3,
            OnLeave = 4,
            Holiday = 5,
            Weekend = 6
        }

        public enum LeaveType
        {
            Annual = 1,
            Sick = 2,
            Unpaid = 3,
            Emergency = 4,
            Maternity = 5,
            Paternity = 6,
            Study = 7
        }

        public enum LeaveRequestStatus
        {
            Pending = 1,
            Approved = 2,
            Rejected = 3,
            Cancelled = 4
        }

        public enum PayrollStatus
        {
            Draft = 1,
            Processed = 2,
            Paid = 3
        }

        public enum PaymentMethod
        {
            BankTransfer = 1,
            Cash = 2,
            Check = 3
        }

        public enum PayrollLineItemType
        {
            Allowance = 1,
            Deduction = 2
        }
}
