using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KernelDatabase.Models;

public enum UserStatus { OnApprove = 0, Approved, Blocked }

[Table("Users", Schema = "OUT_PERS")]
public class FarmerUser
{
    public const string RoleAdmin = "admin";
    public const string RoleCommon = "none";

    public int Id { get; set; }
    public int LegalPersonId { get; set; }
    public string Mail { get; set; } = default!;
    public UserStatus Status { get; set; }
    public string? DisplayName { get; set; }
    public string? Job { get; set; }
    public string? Phone { get; set; }
    public string? Role { get; set; }
    public long? TelegramId { get; set; }

    [NotMapped]
    public string? FixedPhoneNumber
    {
        get  
        {
            if (Phone == null)
                return null;

            char[] fixedPhone = new char[Phone.Length];
            int counter = 0;
            foreach (char c in Phone)
            {
                if (c != ' ' && c != '(' && c != ')' && c != '-')
                {
                    fixedPhone[counter++] = c;
                }
            }

            return new string(fixedPhone, 0, counter);
        }
    }
}
