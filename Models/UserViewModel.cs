using System;
using System.Collections.Generic;

namespace PGSA_Licence3.Models
{
    public class UserViewModel
    {  public int Id { get; set; }
    public string Type { get; set; } = "";
    public string Nom { get; set; } = "";
    public string Prenom { get; set; } = "";
    public string Email { get; set; } = "";
    public bool Active { get; set; }
    public string? Cycle { get; set; }
    public string? Niveau { get; set; }
    public string? Specialite { get; set; }
    }
}