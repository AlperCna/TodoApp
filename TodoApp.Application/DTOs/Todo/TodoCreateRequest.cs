using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoApp.Application.DTOs.Todo;


public record TodoCreateRequest(string Title, string? Description, DateTime? DueDate);