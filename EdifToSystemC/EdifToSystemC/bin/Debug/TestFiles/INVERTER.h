#include "systemc.h"
SC_MODULE(inv) {
	sc_in<bool> I;
	sc_out<bool> O;

	void do_inv() {
		//---- Добавьте описание поведенческой модели модуля inv ----
	}

	SC_CTOR(inv) {
		SC_METHOD(do_inv);
		sensitive << I;
	}
};

SC_MODULE(INVERTER) {
	sc_in<bool> A;
	sc_out<bool> B;
	inv U1;

	SC_CTOR(INVERTER): U1("U1_lbl") {
		U1.I(A); 
		U1.O(B); 
	}
};

