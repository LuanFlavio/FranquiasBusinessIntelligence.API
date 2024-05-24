using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain.Data
{
    [Table("Usuario")]
    public class IUser
    {
        [Key]
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("name")]
        public string? Nome { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Senha { get; set; } = string.Empty;

        [JsonPropertyName("profile")]
        public string? Perfil { get; set; }

        [JsonPropertyName("company")]
        public string Empresa { get; set; } = string.Empty;

    }

    public class UsuariosLogin
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
