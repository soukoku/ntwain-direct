using System;
using System.Collections.Generic;
using System.Text;

namespace DSBridge;

public record DSRequest
{
    public required string Command { get; init; }
}
