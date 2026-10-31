using Joseco.DDD.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.Domain.Transactions.Events;

public record TransactionItemAdded(Guid ItemId) : DomainEvent;
