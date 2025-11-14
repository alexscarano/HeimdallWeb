using System.ComponentModel.DataAnnotations;

namespace HeimdallWeb.Models;

public class LogModel
{
    [Key]
    public int log_id { get; set; }

    /// <summary>
    /// Data e hora do evento
    /// </summary>
    [Required]
    public DateTime timestamp { get; set; } = DateTime.Now;

    /// <summary>
    /// Nível do log: Info, Warning, Error, Critical, Debug
    /// </summary>
    [MaxLength(10)]
    [Required]
    public string level { get; set; } = "Info";

    /// <summary>
    /// Origem do log (ex: ScannerManager, UserController, GeminiService, etc)
    /// </summary>
    [MaxLength(100)]
    public string? source { get; set; }

    /// <summary>
    /// Mensagem principal
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string message { get; set; } = null!;

    /// <summary>
    /// Detalhes técnicos (stack trace, exceção, etc)
    /// </summary>
    public string? details { get; set; }

    /// <summary>
    /// ID do usuário relacionado (se aplicável)
    /// </summary>
    public int? user_id { get; set; }

    /// <summary>
    /// ID do histórico relacionado (se aplicável)
    /// </summary>
    public int? history_id { get; set; }

    /// <summary>
    /// IP remoto
    /// </summary>
    public string? remote_ip { get; set; }

    // navigation properties could be added here if needed
    public HistoryModel? History { get; set; }

    public UserModel? User { get; set; }
}
