using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KernelDatabase.Models;
using Microsoft.EntityFrameworkCore;

namespace FarmerTelegramService.CommandHandlers.MessageChains;

internal static class MessageChainsHelper
{
    private static readonly Dictionary<ChainsCodes, IMessageChainManager?> _messageChains = ReadMessageChains();

    internal static void ClearMessagesChain(KernelDbContext dbCtx, long chatId)
    {
        foreach (var chain in dbCtx.MessageChains.Where(i => i.ChatId == chatId).ToList())
        {
            dbCtx.MessageChains.Remove(chain);
        }
        dbCtx.SaveChanges();
        
        //dbCtx.Database.ExecuteSqlRaw($"delete Kernel.TG.MessageChains where ChatId = {chatId}");
    }

    internal static IMessageChainManager GetChainManager(ChainsCodes chainCode)
    {
        if (!_messageChains.ContainsKey(chainCode) || _messageChains[chainCode] is null)
            throw new ApplicationException($"IMessageChain is not implemented for '{chainCode}'");

        return _messageChains[chainCode]!;
    }

    internal static (IMessageChainManager?, MessageChainLink?) GetCurrentChainManager(KernelDbContext dbCtx, long chatId)
    {
        var lastChainLink = dbCtx.MessageChains.Where(i => i.ChatId == chatId).OrderByDescending(i => i.Id).FirstOrDefault();

        if (lastChainLink is null)
        {
            return (null, null);
        }

        ChainsCodes chainCode = (ChainsCodes)lastChainLink.ChainCode;
        if (!_messageChains.ContainsKey(chainCode) || _messageChains[chainCode] is null)
            throw new ApplicationException($"IMessageChain is not implemented for '{chainCode}'");

        return (_messageChains[chainCode], lastChainLink);
    }

    private static Dictionary<ChainsCodes, IMessageChainManager?> ReadMessageChains()
    {
        var res = new Dictionary<ChainsCodes, IMessageChainManager?>();

        var classesList = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
            .Where(p => typeof(IMessageChainManager).IsAssignableFrom(p)).ToList();

        foreach (var assembly in classesList)
        {
            foreach (var attr in assembly.GetCustomAttributes(false))
            {
                if (attr is ChainsCodeAttribute codeAttr)
                {
                    res.Add(codeAttr.ChainCode, (IMessageChainManager?)Activator.CreateInstance(assembly));
                }
            }
        }

        return res;
    }
}
