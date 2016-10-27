event a;
main machine Main {
    start state S {
        entry {
            receive {
                case a : {}
            }
            
            receive {
                case a : {}
            }
        }
    }
}
