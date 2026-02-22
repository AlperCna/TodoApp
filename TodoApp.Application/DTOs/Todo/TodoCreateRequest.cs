using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace TodoApp.Application.DTOs.Todo;

public record TodoCreateRequest(
    [Required(ErrorMessage = "Başlık boş olamaz")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Başlık 1-200 karakter arasında olmalıdır")]
    string Title,

    [MaxLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
    string? Description,

    DateTime? DueDate
);