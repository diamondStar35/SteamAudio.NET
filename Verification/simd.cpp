// Numerical regression for the unaligned accumulation path corrected by the attributed NVGT patch.
#include "array_math.h"
#include <cmath>
#include <cstdio>
#include <complex>
int main() {
    alignas(64) ipl::complex_t a[40], b[40], out[40];
    for (int i=0;i<40;++i) { a[i]={.2f,.3f}; b[i]={.7f,-.1f}; out[i]={.1f,.2f}; }
    ipl::ArrayMath::multiplyAccumulate(17,a+1,b+1,out+1);
    const auto expected=ipl::complex_t(.1f,.2f)+ipl::complex_t(.2f,.3f)*ipl::complex_t(.7f,-.1f);
    float error=0; for(int i=1;i<=17;++i) error=std::fmax(error,std::abs(out[i]-expected));
    std::printf("Unaligned complex accumulation: max error %.9g\n",error);
    return error<1e-5f ? 0 : 1;
}
