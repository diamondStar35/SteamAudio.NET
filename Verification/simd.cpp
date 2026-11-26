// Numerical regression for the unaligned accumulation path corrected by the attributed NVGT patch.
#include <cmath>
#include <cstdio>
#include <complex>
// Mirror the namespace function declaration in the pinned SDK's array_math.h/types.h.
// This probe links the real archive implementation. It needs no extracted build tree,
// which vcpkg deliberately omits when restoring a compiled binary from its cache.
namespace ipl {
using complex_t = std::complex<float>;
namespace ArrayMath {
void multiplyAccumulate(int size, const complex_t* in1, const complex_t* in2, complex_t* accum);
}
}
int main() {
    alignas(64) ipl::complex_t a[40], b[40], out[40];
    for (int i=0;i<40;++i) { a[i]={.2f,.3f}; b[i]={.7f,-.1f}; out[i]={.1f,.2f}; }
    ipl::ArrayMath::multiplyAccumulate(17,a+1,b+1,out+1);
    const auto expected=ipl::complex_t(.1f,.2f)+ipl::complex_t(.2f,.3f)*ipl::complex_t(.7f,-.1f);
    float error=0; for(int i=1;i<=17;++i) error=std::fmax(error,std::abs(out[i]-expected));
    std::printf("Unaligned complex accumulation: max error %.9g\n",error);
    return error<1e-5f ? 0 : 1;
}
