#include <string>
#include <sstream>
#include <iostream>
#include "meddly.h"
#include "meddly.hh"
#include "meddly_expert.h"
#include "meddly_expert.hh"

using namespace MEDDLY;

extern "C" {
void meddly_init(int num_vars);
void meddly_close();
int allocate_variable(int bound, char* name);
dd_edge* create_edge_for_var(int var, int which_term);
dd_edge* mdd_and(dd_edge* a, dd_edge* b);
dd_edge* mdd_or(dd_edge* a, dd_edge* b);
dd_edge* mdd_not(dd_edge* a);
bool mdd_equalequal(dd_edge* a, dd_edge *b);
dd_edge* get_mdd_true();
dd_edge* get_mdd_false();
void mdd_free(dd_edge* d);
int get_num_vars();
char* edge_to_string(dd_edge* d);
void delete_string(char*);
}

static expert_domain* d;
static expert_forest* f;
static int allocated_var = 0;

void 
meddly_init(int num_vars)
{
	initialize();
	int bounds[num_vars+1];
	for(int i=0 ; i < num_vars; i++)
	{
		bounds[i] = 2;
	}
	forest::policies p(false);
	p.setOptimistic();
	p.setFullyReduced();
	d = static_cast<expert_domain*>(createDomainBottomUp(bounds, num_vars));
	f = dynamic_cast <expert_forest*> (
		d->createForest(false, forest::BOOLEAN, forest::MULTI_TERMINAL, p)
	);
}

void
meddly_close()
{
	cleanup();
}

int allocate_variable(int bound, char* _name)
{
	int new_var = ++allocated_var;
	d->enlargeVariableBound(new_var, false, bound);
	char* name = new char[strlen(_name)]; // no variable is ever destroyed so no need to free
	strcpy(name, _name);
	d->useVar(new_var)->setName(name);
	return new_var - 1;
}

dd_edge* create_edge_for_var(int var, int which_term)
{
	int bound = d->getVariableBound(var + 1, false);
	bool terms[bound];
	memset(terms, 0, sizeof(bool) * bound);
	terms[which_term] = true;
	dd_edge* node = new dd_edge(f);
	f->createEdgeForVar(var + 1, false, terms, *node);
	// std::cout << "---\n" << edge_to_string(node) << std::endl;
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
	f->createEdge(false, *mdd_false);
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

char* edge_to_string(dd_edge* d)
{
	std::ostringstream str_buffer;
	ostream_output out(str_buffer);
	d->show(out, 2);
	std::string s = str_buffer.str();
	char* ret = new char[s.length()];
	strcpy(ret, s.c_str());
	return ret;
}

void delete_string(char* s)
{
	delete[] s;
}

