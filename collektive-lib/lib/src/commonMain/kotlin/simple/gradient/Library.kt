package simple.gradient

import it.unibo.collektive.aggregate.api.Aggregate
import it.unibo.collektive.aggregate.api.share
import it.unibo.collektive.aggregate.values
import it.unibo.collektive.stdlib.collapse.min

fun Aggregate<Int>.hopCountGradient(source: Boolean): Int =
    share(Int.MAX_VALUE) { field ->
        val bestNeighbor = field.neighbors.values.min()
        val result = if (bestNeighbor == Int.MAX_VALUE) {
            Int.MAX_VALUE
        } else {
            bestNeighbor + 1
        }
        when {
            source -> 0
            else -> result
        }
    }
