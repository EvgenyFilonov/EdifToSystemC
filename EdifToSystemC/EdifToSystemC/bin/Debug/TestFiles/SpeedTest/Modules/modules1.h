#include "systemc.h"
SC_MODULE(nand2) {
	sc_in<bool> I0, I1;
	sc_out<bool> O;

	void do_nand2() {
		//---- Добавьте описание поведенческой модели модуля nand2 ----
	}

	SC_CTOR(nand2) {
		SC_METHOD(do_nand2);
		sensitive << I0 << I1;
	}
};

SC_MODULE(EXOR) {
	sc_in<bool> A, B;
	sc_out<bool> F;
	nand2 U1, U2, U3, U4;
	sc_signal<bool> NET92, NET84, NET43;

	SC_CTOR(EXOR): U1(U1_lbl), U2(U2_lbl), U3(U3_lbl), U4(U4_lbl) {
		U1.I1(A); U3.I1(A); 
		U2.I0(B); U3.I0(B); 
		U1.O(NET92); U4.I1(NET92); 
		U2.O(NET84); U4.I0(NET84); 
		U4.O(F); 
		U1.I0(NET43); U2.I1(NET43); U3.O(NET43); 
	}
};

