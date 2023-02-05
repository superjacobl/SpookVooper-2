﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV2.Scripting;

public enum EffectType
{
	None,
	AddModifier,
	AddMoney
}

public abstract class EffectSyntaxNode : SyntaxNode
{
	public NodeType NodeType => NodeType.EFFECT;
	public EffectType effectType { get; set; }
	public override decimal GetValue(District district) => 0.00m;
	public abstract void Execute(District district);
}

public interface IEffectNode
{
	public EffectType effectType { get; }
	public abstract void Execute(District district);
}

public class EffectNode : EffectSyntaxNode, IEffectNode
{
	public override void Execute(District district)
	{
		throw new NotImplementedException();
	}
}

public class AddMoneyNode : EffectNode
{
	public EffectType effectType => EffectType.AddMoney;
	public SyntaxNode Amount { get; set; }

	public override void Execute(District district)
	{
		var tran = new Transaction(100, district.GroupId, Amount.GetValue(district), TransactionType.FreeMoney, "From Effect Node");
		tran.NonAsyncExecute();
		Console.WriteLine("Executed AddMoneyNode!");
	}
}

public class AddModifierNode : EffectNode
{
	public EffectType effectType => EffectType.AddModifier;
	public string ModifierName { get; set; }
	public bool Decay { get; set; } = false;
	public int Duration { get; set; }
	public SyntaxNode ScaleBy { get; set; }

	public override void Execute(District district)
	{
		var dbmodifier = new DistrictStaticModifier()
		{
			Id = StaticModifierManager.idManager.Generate(),
			DistrictId = district.Id,
			Decay = Decay,
			Duration = Duration,
			StartDate = DateTime.UtcNow,
			ScaleBy = ScaleBy.GetValue(district),
			StaticModifierId = ModifierName
		};
		using var dbctx = VooperDB.DbFactory.CreateDbContext();
		dbctx.DistrictStaticModifiers.Add(dbmodifier);
		dbctx.SaveChanges();
		DBCache.Put(dbmodifier.Id, dbmodifier);
		dbmodifier.District = district;
	}
}

public class EffectBody : SyntaxNode
{
	public List<IEffectNode> Body;
	public EffectBody()
	{
		NodeType = NodeType.EFFECTBODY;
		Body = new();
	}

	public void Execute(District district)
	{
		foreach(var effectnode in Body)
			effectnode.Execute(district);
	}

	public override decimal GetValue(District district)
	{
		throw new NotImplementedException();
	}

	public static explicit operator EffectBody(ExpressionNode v)
	{
		return new EffectBody()
		{
			Body = v.Body.Select(x => (IEffectNode)x).ToList(),
			NodeType = NodeType.EFFECTBODY
		};
	}
}