// Regression for unaligned complex multiply-accumulate.
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
int main()
{
    alignas(64) ipl::complex_t left[40], right[40], output[40];
    for (int index = 0; index < 40; ++index)
    {
        left[index] = {.2f, .3f};
        right[index] = {.7f, -.1f};
        output[index] = {.1f, .2f};
    }

    ipl::ArrayMath::multiplyAccumulate(17, left + 1, right + 1, output + 1);
    const auto expected = ipl::complex_t(.1f, .2f)
        + ipl::complex_t(.2f, .3f) * ipl::complex_t(.7f, -.1f);

    float error = 0;
    for (int index = 1; index <= 17; ++index)
    {
        error = std::fmax(error, std::abs(output[index] - expected));
    }

    std::printf("Unaligned complex accumulation: max error %.9g\n", error);
    return error < 1e-5f ? 0 : 1;
}
