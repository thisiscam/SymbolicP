using System;
using SCG = System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#if USE_SYLVAN
using bdd = SylvanSharp.bdd;
using BDDLIB = SylvanSharp.SylvanSharp;
#else
using bdd = BuDDySharp.bdd;
using BDDLIB = BuDDySharp.BuDDySharp;
#endif

public class VSSet<T>
{
	private SCG.Dictionary<T, bdd> data = new SCG.Dictionary<T, bdd>();
	private ValueSummary<int> _count = 0;
	
	public VSSet()
	{
		
	}
	
	public void Add(ValueSummary<T> item)
	{
		var pc = bdd.bddtrue;
		foreach(var guardedvalue in item.values)
		{
			var bddForm = pc.And(guardedvalue.bddForm);
			if(bddForm.FormulaBDDSAT()) {
				if(data.ContainsKey(guardedvalue.value)) {
					PathConstraint.AddAxiom(data[guardedvalue.value].Not().And(bddForm));
					data[guardedvalue.value] = data[guardedvalue.value].Or(bddForm);
					_count.Increment();
					PathConstraint.RestorePC(pc);
				} else {
					PathConstraint.AddAxiom(guardedvalue.bddForm);
					data.Add(guardedvalue.value, bddForm);
					_count.Increment();
					PathConstraint.RestorePC(pc);
				}
			}
		}
	}
	
	public ValueSummary<int> Count
    {
        get
        {
            return ValueSummary<int>.InitializeFrom(_count);
        }
    }
    
    public void ForEach(Action<ValueSummary<T>> f)
    {
    	var pc = PathConstraint.GetPC();
    	foreach(var guardedData in this.data)
    	{
    		var bddForm = guardedData.Value.And(pc);
    		if(bddForm.FormulaBDDSolverSAT()) {
    			PathConstraint.AddAxiom(guardedData.Value);
    			var v = new ValueSummary<T>();
    			v.AddValue(bddForm, guardedData.Key);
    			f.Invoke(v);
    			PathConstraint.RestorePC(pc);
    		}
    	}
    }
}

