using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarmerTelegramService.Entities;
using KernelDatabase.Models;

namespace FarmerTelegramService.CommandHandlers.MessageChains;

public interface IMessageChainManager
{
    Task ProcessInputAsync(KernelDbContext dbCtx, AppMiddleware appMiddleware, AbstractCommandHandler commandHandler, 
		MessageChainLink lastChainLink, string? command);
	
	Task StartChain(KernelDbContext dbCtx, AppMiddleware appMiddleware, AbstractCommandHandler commandHandler, long chatId);
}

public enum ChainsCodes { DocSearch }

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ChainsCodeAttribute : Attribute
{
	public ChainsCodeAttribute(ChainsCodes chainCode)
	{
		ChainCode = chainCode;
	}

	public ChainsCodes ChainCode { get; }
}