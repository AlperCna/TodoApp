using AngleSharp.Css.Values;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoApp.Application.DTOs.Todo;

public record TodoUpdateRequest(
    [Required(ErrorMessage = "Başlık boş olamaz")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Başlık 1-200 karakter arasında olmalıdır")]
    string Title,

    [MaxLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
    string? Description,

    bool IsCompleted,
    DateTime? DueDate
); 