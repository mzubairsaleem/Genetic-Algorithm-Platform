"use strict";
function isPrime(n) {
    if (n == 2 || n == 3)
        return true;
    if (n % 2 == 0 || n % 3 == 0)
        return false;
    var divisor = 6;
    while (divisor * divisor - 2 * divisor + 1 <= n) {
        if (n % (divisor - 1) == 0)
            return false;
        if (n % (divisor + 1) == 0)
            return false;
        divisor += 6;
    }
    return true;
}
exports.isPrime = isPrime;
function nextPrime(a) {
    while (!isPrime(++a)) { }
    return a;
}
exports.nextPrime = nextPrime;
//# sourceMappingURL=Primes.js.map