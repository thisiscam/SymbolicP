#include "meddly.h"
#include "meddly.hh"
#include "meddly_expert.h"
#include "meddly_expert.hh"

using namespace MEDDLY;

extern "C" {
void meddly_init(int num_vars);
void meddly_close();
void allocate_variable(int bound, char* name);
dd_edge* create_edge_for_var(int var, const bool* terms);
dd_edge* mdd_and(dd_edge* a, dd_edge* b);
dd_edge* mdd_or(dd_edge* a, dd_edge* b);
dd_edge* mdd_not(dd_edge* a);
bool mdd_equalequal(dd_edge* a, dd_edge *b);
dd_edge* get_mdd_true();
dd_edge* get_mdd_false();
void mdd_free(dd_edge* d);
int get_num_vars();
void debug_edge(dd_edge* d);
}

static expert_domain* d;
static forest* f;
static int allocated_var = 0;

void 
meddly_init(int num_vars)
{
	initialize();
	int bounds[num_vars];
	for(int i=0 ; i < num_vars; i++)
	{
		bounds[i] = 2;
	}
	d = static_cast<expert_domain*>(createDomainBottomUp(bounds, num_vars));
	f = d->createForest(false, forest::BOOLEAN, forest::MULTI_TERMINAL);
}

void
meddly_close()
{
	cleanup();
}

void allocate_variable(int bound, char* name)
{
	int new_var = ++allocated_var;
	d->enlargeVariableBound(new_var, false, bound);
	d->useVar(new_var)->setName(name);
}

dd_edge* create_edge_for_var(int var, const bool* terms)
{
	dd_edge* node = new dd_edge(f);
	f->createEdgeForVar(var + 1, false, terms, *node);
	return node;
}

dd_edge* mdd_and(dd_edge* a, dd_edge* b)
{
	dd_edge* node = new dd_edge(f);
	apply(INTERSECTION, *a, *b, *node);
	return node;
}

dd_edge* mdd_or(dd_edge* a, dd_edge* b)
{
	dd_edge* node = new dd_edge(f);
	apply(UNION, *a, *b, *node);
	return node;
}

dd_edge* mdd_not(dd_edge* a)
{
	dd_edge* node = new dd_edge(f);
	apply(COMPLEMENT, *a, *node);
	return node;
}

dd_edge* get_mdd_true()
{
	dd_edge* mdd_true = new dd_edge(f);
	f->createEdge(true, *mdd_true);
	return mdd_true;
}

dd_edge* get_mdd_false()
{
	dd_edge* mdd_false = new dd_edge(f);
	f->createEdge(true, *mdd_false);
	return mdd_false;
}

bool mdd_equalequal(dd_edge* a, dd_edge *b)
{
	return *a == *b;
}

void mdd_free(dd_edge* d)
{
	delete d;
}

int get_num_vars()
{
	return allocated_var;
}

void debug_edge(dd_edge* d)
{
	ostream_output out(std::cout);
	d->show(out, 2);
}